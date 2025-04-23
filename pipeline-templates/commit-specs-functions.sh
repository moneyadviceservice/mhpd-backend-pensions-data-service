#!/bin/bash

echo "SPEC_FILE_NAME: $SPEC_FILE_NAME"
echo "TARGET_DIR: wwwroot/swagger/v1/$SPEC_FILE_NAME"
echo "BRANCH_NAME: $BRANCH_NAME"
echo "SERVICE_NAME: $SERVICE_NAME"
echo "SERVICE_PATH: $SERVICE_PATH"
echo "FUNCTION_NAME: $FUNCTION_NAME"

TARGET_DIR="wwwroot/swagger/v1/$SPEC_FILE_NAME"

export AzureWebJobsStorage="UseDevelopmentStorage=true"
export FUNCTIONS_WORKER_RUNTIME="dotnet-isolated"
export FUNCTIONS_INPROC_NET8_ENABLED="true"

cd $SERVICE_PATH/$SERVICE_NAME/app/$FUNCTION_NAME

npm i -g azure-functions-core-tools@4 --unsafe-perm true
pwd
ls
dotnet build

func start --dotnet-isolated --no-timeout &  
sleep 10

# Wait until the function is actually running
until curl --output $TARGET_DIR http://localhost:7071/swagger.json; do  
    echo "Waiting for function to start..."  
    sleep 2  
done

if [[ -n $(git status --porcelain "$TARGET_DIR") ]]; then
    echo "🔍 Changes detected in $TARGET_DIR. Committing..."

    # Configure Git
    git config user.email "NotificationBot@maps.org.uk"
    git config user.name "Notification Bot"

    # Commit and push changes
    git pull origin $BRANCH_NAME
    git add wwwroot/swagger/v1/
    git commit -m "Auto-update OpenAPI spec"
    git push https://$PAT_TOKEN@dev.azure.com/moneyandpensionsservice/MaPS%20Digital/_git/mhpd-backend HEAD:refs/heads/$BRANCH_NAME
else
    echo "✅ No changes detected. Skipping commit."
fi