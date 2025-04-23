#!/bin/bash

git config --global user.email "NotificationBot@maps.org.uk"
git config --global user.name "Notification Bot"
export AZURE_DEVOPS_EXT_PAT=$PAT_TOKEN

BRANCH_NAME="update-spec-$(cat /dev/urandom | tr -dc 'a-z0-9' | fold -w 6 | head -n 1)"

cd shared-infrastructure/components/apim
current_revision=$(grep 'revision' $TF_FILE | awk -F'= ' '{print $2}' | tr -d '"')

next_revision=$((current_revision + 1))
echo "Current API Revision: $current_revision"
echo "Next API Revision: $next_revision"

awk -v new_revision="$next_revision" '/revision[[:space:]]*=/ {sub(/"[^"]*"/, "\"" new_revision "\"")} 1' $TF_FILE > temp && mv temp $TF_FILE

git checkout -b $BRANCH_NAME
git add $TF_FILE
git commit -m "Bump API revision to $next_revision"
git push https://$PAT_TOKEN@dev.azure.com/moneyandpensionsservice/MaPS%20Digital/_git/mhpd-backend HEAD:refs/heads/$BRANCH_NAME
az repos pr create --repository "mhpd-backend" --target-branch develop --source-branch $BRANCH_NAME --title "Update OpenAPI spec"
