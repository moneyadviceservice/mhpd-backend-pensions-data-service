#!/bin/bash

TARGET_DIR="$SERVICE_NAME/wwwroot/swagger/v1/"
PAT_TOKEN="${PAT_TOKEN}"
BRANCH_NAME="${BRANCH_NAME}"
SERVICE_NAME="${SERVICE_NAME}"
SERVICE_PATH="${SERVICE_PATH}"

cd $SERVICE_PATH/$SERVICE_NAME/app/$SERVICE_NAME

dotnet tool list -g
dotnet tool restore
dotnet restore
dotnet build

# Check for changes
if [[ -n $(git status --porcelain "$TARGET_DIR") ]]; then
    echo "🔍 Changes detected in $TARGET_DIR. Committing..."

    # Configure Git
    git config user.email "NotificationBot@maps.org.uk"
    git config user.name "Notification Bot"

    # Commit and push changes
    git add $SERVICE_NAME/wwwroot/swagger/v1/
    git commit -m "Auto-update OpenAPI spec"
    git push https://$PAT_TOKEN@dev.azure.com/moneyandpensionsservice/MaPS%20Digital/_git/mhpd-backend HEAD:refs/heads/$BRANCH_NAME
else
    echo "✅ No changes detected. Skipping commit."
fi