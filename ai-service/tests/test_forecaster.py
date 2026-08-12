import pytest
import datetime
from models.forecaster import DemandForecaster

@pytest.fixture
def forecaster():
    return DemandForecaster()

def generate_mock_sales(days, base_quantity=10, drop_weekends=False):
    start_date = datetime.date.today() - datetime.timedelta(days=days)
    data = []
    for i in range(days):
        current_date = start_date + datetime.timedelta(days=i)
        q = base_quantity
        if drop_weekends and current_date.weekday() >= 5:
            q = 0
        data.append({"date": current_date.isoformat(), "quantity": q})
    return data

def test_insufficient_data(forecaster):
    result = forecaster.forecast([], 0)
    assert result["modelUsed"] == "insufficient_data"
    assert result["forecast7Days"] == 0

def test_cold_start_1_to_6_days(forecaster):
    data = generate_mock_sales(5, base_quantity=10)
    result = forecaster.forecast(data, current_stock=0)
    assert result["modelUsed"] == "moving_average"
    assert result["confidence"] == "Low"
    assert result["forecast7Days"] == 70.0  # 10 * 7
    assert "metrics" in result

def test_moving_average_7_to_44_days(forecaster):
    data = generate_mock_sales(30, base_quantity=10)
    result = forecaster.forecast(data, current_stock=0)
    assert result["modelUsed"] == "moving_average"
    assert result["confidence"] == "Medium"
    assert result["forecast7Days"] == 70.0
    assert "mae" in result["metrics"]

def test_ml_model_sufficient_data(forecaster):
    # Need >45 rows for ML branch
    data = generate_mock_sales(60, base_quantity=20, drop_weekends=True)
    result = forecaster.forecast(data, current_stock=100)
    assert result["modelUsed"] in ["moving_average", "random_forest", "gradient_boosting"]
    assert result["recommendedPurchaseQuantity"] >= 0
    assert result["metrics"]["rmse"] >= 0
    assert result["metrics"]["mape"] >= 0

def test_negative_forecast_prevention(forecaster):
    # Pass 0 stock, negative input should be clipped
    data = generate_mock_sales(60, base_quantity=-10)
    result = forecaster.forecast(data, current_stock=0)
    assert result["forecast7Days"] == 0.0
    assert result["recommendedPurchaseQuantity"] >= 0

def test_all_zero_sales(forecaster):
    data = generate_mock_sales(50, base_quantity=0)
    result = forecaster.forecast(data, current_stock=5)
    assert result["forecast7Days"] == 0.0
    assert result["recommendedPurchaseQuantity"] == 0
