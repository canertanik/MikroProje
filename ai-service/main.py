from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List
from models.forecaster import DemandForecaster

app = FastAPI(title="MikroProje AI Demand Forecast Service")

class SaleData(BaseModel):
    date: str
    quantity: float

class ForecastRequest(BaseModel):
    productId: int
    currentStock: int
    sales: List[SaleData]

from typing import List, Optional

class Metrics(BaseModel):
    mae: Optional[float] = None
    rmse: Optional[float] = None
    mape: Optional[float] = None

class ForecastResponse(BaseModel):
    forecast7Days: float
    forecast30Days: float
    averageDailyDemand: float
    estimatedStockDays: int
    riskLevel: str
    recommendedPurchaseQuantity: int
    modelUsed: str
    confidence: str
    metrics: Metrics
    message: str

forecaster = DemandForecaster()

@app.get("/health")
def health_check():
    return {"status": "ok"}

@app.post("/api/forecast", response_model=ForecastResponse)
def predict_demand(request: ForecastRequest):
    try:
        # Convert Pydantic models to dicts for pandas
        sales_dicts = [{"date": s.date, "quantity": s.quantity} for s in request.sales]
        
        result = forecaster.forecast(sales_data=sales_dicts, current_stock=request.currentStock)
        
        return ForecastResponse(**result)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
