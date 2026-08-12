import pandas as pd
import numpy as np
from sklearn.ensemble import RandomForestRegressor, GradientBoostingRegressor
from sklearn.metrics import mean_absolute_error, root_mean_squared_error
import math

def calculate_mape(y_true, y_pred):
    y_true, y_pred = np.array(y_true), np.array(y_pred)
    # Mask actual == 0 observations to prevent artificially huge MAPE values
    mask = y_true != 0
    if not np.any(mask):
        return 0.0 # If all actuals are 0, MAPE is technically undefined, return 0.0 or a custom value
    return np.mean(np.abs((y_true[mask] - y_pred[mask]) / y_true[mask])) * 100

class DemandForecaster:
    def __init__(self):
        pass

    def _prepare_features(self, df: pd.DataFrame) -> pd.DataFrame:
        df['date'] = pd.to_datetime(df['date'])
        df = df.sort_values('date').reset_index(drop=True)
        
        if len(df) > 0:
            # Group by date to prevent duplicate index errors when there are multiple sales on the same day
            df = df.groupby('date', as_index=False)['quantity'].sum()
            date_range = pd.date_range(start=df['date'].min(), end=df['date'].max(), freq='D')
            df = df.set_index('date').reindex(date_range).fillna(0).rename_axis('date').reset_index()

        df['dayOfWeek'] = df['date'].dt.dayofweek
        df['month'] = df['date'].dt.month
        
        # Shift(1) to avoid data leakage (using future/current data for predicting current day)
        df['shifted_qty'] = df['quantity'].shift(1).fillna(0)
        
        df['lag1'] = df['shifted_qty']
        df['lag7'] = df['quantity'].shift(7).fillna(0)
        df['lag14'] = df['quantity'].shift(14).fillna(0)
        df['lag30'] = df['quantity'].shift(30).fillna(0)
        
        # Rolling on shifted quantity to avoid including the target day's quantity!
        df['rolling7'] = df['shifted_qty'].rolling(window=7, min_periods=1).mean().fillna(0)
        df['rolling30'] = df['shifted_qty'].rolling(window=30, min_periods=1).mean().fillna(0)
        
        return df

    def forecast(self, sales_data: list, current_stock: int = 0) -> dict:
        metrics = {"mae": None, "rmse": None, "mape": None}
        
        if not sales_data or len(sales_data) == 0:
            return self._build_response(0, 0, 0, current_stock, "insufficient_data", "Low", metrics, "Tahmin i\u00e7in yeterli veri yok.")

        df = pd.DataFrame(sales_data)
        # Prevent negative quantity input
        df['quantity'] = df['quantity'].clip(lower=0)
        
        df = self._prepare_features(df)
        total_rows = len(df)
        valid_train_rows = max(0, total_rows - 30) # Account for lag30 loss

        average_daily_demand = max(0, float(df['quantity'].mean()))
        
        # Determine confidence rules
        # High: >45 days of data and best RMSE is good (implicitly checked later)
        # Medium: 7-44 days, or MA fallback
        # Low: < 7 days
        
        if total_rows < 7:
            forecast7 = average_daily_demand * 7
            forecast30 = average_daily_demand * 30
            return self._build_response(forecast7, forecast30, average_daily_demand, current_stock, "moving_average", "Low", metrics, "Basic daily average due to <7 days data.")
            
        elif total_rows < 45 or valid_train_rows < 7:
            recent_mean = max(0, float(df['quantity'].tail(7).mean()))
            forecast7 = recent_mean * 7
            forecast30 = recent_mean * 30
            return self._build_response(forecast7, forecast30, average_daily_demand, current_stock, "moving_average", "Medium", metrics, "Moving average due to limited data.")
            
        else:
            # Chronological split without shuffling
            train_df = df.iloc[:-7]
            val_df = df.iloc[-7:]
            
            features = ['dayOfWeek', 'month', 'lag1', 'lag7', 'lag14', 'lag30', 'rolling7', 'rolling30']
            X_train = train_df[features]
            y_train = train_df['quantity']
            X_val = val_df[features]
            y_val = val_df['quantity']
            
            # Baseline: MA of last 7 days of train
            ma_pred = train_df['quantity'].tail(7).mean()
            ma_preds = np.full(len(y_val), ma_pred)
            ma_rmse = root_mean_squared_error(y_val, ma_preds)
            ma_mae = mean_absolute_error(y_val, ma_preds)
            ma_mape = calculate_mape(y_val, ma_preds)
            
            # RandomForest
            rf_model = RandomForestRegressor(n_estimators=50, random_state=42)
            rf_model.fit(X_train, y_train)
            rf_preds = rf_model.predict(X_val)
            rf_rmse = root_mean_squared_error(y_val, rf_preds)
            rf_mae = mean_absolute_error(y_val, rf_preds)
            rf_mape = calculate_mape(y_val, rf_preds)
            
            # GradientBoosting
            gb_model = GradientBoostingRegressor(n_estimators=50, random_state=42)
            gb_model.fit(X_train, y_train)
            gb_preds = gb_model.predict(X_val)
            gb_rmse = root_mean_squared_error(y_val, gb_preds)
            gb_mae = mean_absolute_error(y_val, gb_preds)
            gb_mape = calculate_mape(y_val, gb_preds)
            
            best_rmse = min(ma_rmse, rf_rmse, gb_rmse)
            
            X_full = df[features]
            y_full = df['quantity']
            
            if best_rmse == ma_rmse:
                model_used = "moving_average"
                confidence = "Medium"
                metrics = {"mae": round(ma_mae, 2), "rmse": round(ma_rmse, 2), "mape": round(ma_mape, 2)}
                recent_mean = max(0, float(df['quantity'].tail(7).mean()))
                forecast7 = recent_mean * 7
                forecast30 = recent_mean * 30
                return self._build_response(forecast7, forecast30, average_daily_demand, current_stock, model_used, confidence, metrics, "Selected MA based on validation.")
            else:
                if best_rmse == rf_rmse:
                    model_used = "random_forest"
                    metrics = {"mae": round(rf_mae, 2), "rmse": round(rf_rmse, 2), "mape": round(rf_mape, 2)}
                    final_model = RandomForestRegressor(n_estimators=50, random_state=42)
                else:
                    model_used = "gradient_boosting"
                    metrics = {"mae": round(gb_mae, 2), "rmse": round(gb_rmse, 2), "mape": round(gb_mape, 2)}
                    final_model = GradientBoostingRegressor(n_estimators=50, random_state=42)
                    
                confidence = "High" if best_rmse < average_daily_demand else "Medium"
                final_model.fit(X_full, y_full)
                
                # Recursive 30-day Horizon Forecast
                last_known_date = df['date'].max()
                future_predictions = []
                
                # Keep a running list of quantities to calculate rolling/lags properly
                qty_history = df['quantity'].tolist()
                
                for step in range(1, 31):
                    target_date = last_known_date + pd.Timedelta(days=step)
                    
                    feat_dayOfWeek = target_date.dayofweek
                    feat_month = target_date.month
                    feat_lag1 = qty_history[-1]
                    feat_lag7 = qty_history[-7] if len(qty_history) >= 7 else 0
                    feat_lag14 = qty_history[-14] if len(qty_history) >= 14 else 0
                    feat_lag30 = qty_history[-30] if len(qty_history) >= 30 else 0
                    
                    feat_rolling7 = np.mean(qty_history[-7:]) if len(qty_history) >= 7 else np.mean(qty_history)
                    feat_rolling30 = np.mean(qty_history[-30:]) if len(qty_history) >= 30 else np.mean(qty_history)
                    
                    X_step = pd.DataFrame([{
                        'dayOfWeek': feat_dayOfWeek,
                        'month': feat_month,
                        'lag1': feat_lag1,
                        'lag7': feat_lag7,
                        'lag14': feat_lag14,
                        'lag30': feat_lag30,
                        'rolling7': feat_rolling7,
                        'rolling30': feat_rolling30
                    }])
                    
                    pred = max(0, final_model.predict(X_step)[0])
                    future_predictions.append(pred)
                    qty_history.append(pred) # Feed back for recursive
                    
                forecast7 = sum(future_predictions[:7])
                forecast30 = sum(future_predictions)
                
                return self._build_response(forecast7, forecast30, average_daily_demand, current_stock, model_used, confidence, metrics, "Recursive ML forecast generated.")

    def _build_response(self, f7, f30, avg_daily, current_stock, model_used, confidence, metrics, message):
        if math.isnan(f7) or math.isinf(f7): f7 = 0.0
        if math.isnan(f30) or math.isinf(f30): f30 = 0.0
        
        forecast_daily_demand = f30 / 30.0 if f30 > 0 else avg_daily

        if forecast_daily_demand > 0:
            estimated_stock_days = int(current_stock / forecast_daily_demand)
        else:
            estimated_stock_days = 999

        if estimated_stock_days <= 7:
            risk_level = "Critical"
        elif estimated_stock_days <= 14:
            risk_level = "High"
        elif estimated_stock_days <= 30:
            risk_level = "Medium"
        else:
            risk_level = "Low"

        # Safety Stock formula: fixed 10% of 30-day forecast horizon
        safety_stock = f30 * 0.10
        target_stock = f30 + safety_stock
        
        # Lead time is treated as 0 for V1 limitation. 
        # Recommended Purchase Quantity Formula: MAX(0, Target Stock - Current Stock)
        recommended_purchase = target_stock - current_stock
        recommended_purchase = max(0, int(math.ceil(recommended_purchase)))

        return {
            "forecast7Days": round(float(f7), 2),
            "forecast30Days": round(float(f30), 2),
            "averageDailyDemand": round(float(forecast_daily_demand), 2),
            "estimatedStockDays": estimated_stock_days,
            "riskLevel": risk_level,
            "recommendedPurchaseQuantity": recommended_purchase,
            "modelUsed": model_used,
            "confidence": confidence,
            "metrics": metrics,
            "message": message
        }
