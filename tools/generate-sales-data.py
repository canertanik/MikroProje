import random
import datetime

def generate_synthetic_sales_sql(output_file="synthetic_sales_seed.sql", product_id=1, days=180):
    print(f"Generating synthetic sales data for {days} days for Product ID {product_id}...")
    
    start_date = datetime.date.today() - datetime.timedelta(days=days)
    
    sql_statements = [
        "-- SYNTHETIC SALES DATA FOR DEVELOPMENT & TEST",
        "-- DO NOT RUN THIS IN PRODUCTION!",
        "BEGIN TRANSACTION;",
        "SET IDENTITY_INSERT Products ON;",
        "INSERT INTO Products (Id, Code, Name, Barcode, PurchasePrice, SalePrice, VatRate, StockQuantity, CriticalStockQuantity, CreatedDate, IsDeleted) VALUES (1, 'P01', 'Test Product', '123', 40, 50, 18, 55, 10, GETDATE(), 0);",
        "SET IDENTITY_INSERT Products OFF;",
        "SET IDENTITY_INSERT CurrentAccounts ON;",
        "INSERT INTO CurrentAccounts (Id, Code, Name, Type, Balance, IsDeleted, CreatedDate) VALUES (1, 'C01', 'Test CA', 1, 0.0, 0, GETDATE());",
        "SET IDENTITY_INSERT CurrentAccounts OFF;",
        "SET IDENTITY_INSERT Warehouses ON;",
        "INSERT INTO Warehouses (Id, Code, Name, IsDefault, IsDeleted, CreatedDate) VALUES (1, 'W01', 'Test WH', 1, 0, GETDATE());",
        "SET IDENTITY_INSERT Warehouses OFF;",
        ""
    ]
    
    sale_id_counter = 1000  # Start from a high number to avoid ID collisions
    sale_detail_id_counter = 1000
    
    current_account_id = 1
    warehouse_id = 1
    unit_price = 50.0
    
    for i in range(days):
        current_date = start_date + datetime.timedelta(days=i)
        
        # Simulate weekend drop and some randomness
        base_demand = 20
        if current_date.weekday() >= 5:  # Weekend
            base_demand = 5
            
        # Simulate some out of stock days (0 sales)
        if random.random() < 0.05:  # 5% chance of 0 sales
            quantity = 0
        else:
            quantity = max(0, int(random.normalvariate(base_demand, 5)))
            
        if quantity > 0:
            total_amount = quantity * unit_price
            
            # Insert Sale Header
            sql_statements.append("SET IDENTITY_INSERT Sales ON;")
            sql_statements.append(
                f"INSERT INTO Sales (Id, CurrentAccountId, WarehouseId, SaleDate, TotalAmount, VatAmount, GrandTotal, CreatedDate, IsDeleted) "
                f"VALUES ({sale_id_counter}, {current_account_id}, {warehouse_id}, '{current_date.strftime('%Y-%m-%d')}T00:00:00', {total_amount}, 0, {total_amount}, GETDATE(), 0);"
            )
            sql_statements.append("SET IDENTITY_INSERT Sales OFF;")
            
            # Insert Sale Detail
            sql_statements.append("SET IDENTITY_INSERT SaleDetails ON;")
            sql_statements.append(
                f"INSERT INTO SaleDetails (Id, SaleId, ProductId, Quantity, UnitPrice, VatRate, Discount, LineTotal, CreatedDate, IsDeleted) "
                f"VALUES ({sale_detail_id_counter}, {sale_id_counter}, {product_id}, {quantity}, {unit_price}, 0, 0, {total_amount}, GETDATE(), 0);"
            )
            sql_statements.append("SET IDENTITY_INSERT SaleDetails OFF;")
            
            sale_id_counter += 1
            sale_detail_id_counter += 1

    sql_statements.append("")
    sql_statements.append("COMMIT;")
    
    with open(output_file, 'w') as f:
        f.write("\n".join(sql_statements))
        
    print(f"Success! {len(sql_statements)} lines written to {output_file}.")
    print("Execute this script in your MS SQL Server management tool.")

if __name__ == "__main__":
    generate_synthetic_sales_sql(output_file="seed_dev_sales.sql", product_id=1, days=180)
