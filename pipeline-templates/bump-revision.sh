#!/bin/bash

git config --global user.email "NotificationBot@maps.org.uk"
git config --global user.name "Notification Bot"

cd shared-infrastructure/components/apim
current_revision=$(grep 'revision' $TF_FILE | awk -F'= ' '{print $2}' | tr -d '"')

next_revision=$((current_revision + 1))
echo "Current API Revision: $current_revision"
echo "Next API Revision: $next_revision"

awk -v new_revision="$next_revision" '/revision[[:space:]]*=/ {sub(/"[^"]*"/, "\"" new_revision "\"")} 1' $TF_FILE > temp && mv temp $TF_FILE

git add $TF_FILE
git commit -m "Bump API revision to $next_revision"
git push --force https://$PAT_TOKEN@dev.azure.com/moneyandpensionsservice/MaPS%20Digital/_git/mhpd-backend HEAD:refs/heads/$BRANCH_NAME
