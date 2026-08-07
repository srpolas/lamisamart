#!/bin/bash

# Ensure dotnet ef is installed
dotnet tool install --global dotnet-ef || true
export PATH="$PATH:$HOME/.dotnet/tools"

cd /home/srpolas/Projects/LamisaMart

# Array of modules and their DbContexts
declare -a modules=(
  "Catalog CatalogDbContext LamisaMart.Catalog.Infrastructure"
  "Ordering OrderingDbContext LamisaMart.Ordering.Infrastructure"
  "Payments PaymentsDbContext LamisaMart.Payments.Infrastructure"
  "Vendors VendorsDbContext LamisaMart.Vendors.Infrastructure"
  "Accounting AccountingDbContext LamisaMart.Accounting.Infrastructure"
  "PageBuilder PageBuilderDbContext LamisaMart.PageBuilder.Infrastructure"
)

for val in "${modules[@]}"; do
  set -- $val
  module=$1
  context=$2
  project=$3
  
  echo "Generating migration for $module ($context)..."
  dotnet ef migrations add "Initial$module" -p "src/Modules/$module/$project" -s "src/Web/LamisaMart.Web" -c "$context"
  
  echo "Applying migration for $module ($context)..."
  dotnet ef database update -p "src/Modules/$module/$project" -s "src/Web/LamisaMart.Web" -c "$context"
done

echo "Database migrations applied successfully!"
