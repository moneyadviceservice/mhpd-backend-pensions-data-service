#!/bin/bash

AZURE_ORG="moneyadviceservice"
AZURE_PROJECT="MaPS%20Digital"
AZURE_REPO="mhpd-backend"
AZURE_PAT=$AZURE_PAT

GITHUB_USER="maps-devops-bot"
GITHUB_EMAIL="NotificationBot@maps.org.uk"
GITHUB_REPO="api-docs"
GITHUB_BRANCH="main"
GITHUB_PAT=$GITHUB_PAT

GITHUB_LOCAL_DIR="github_repo"

echo "the service name is $SERVICE_NAME"
echo "the file path is $AZURE_FILE_PATH"

# Check if the file exists
SOURCE_PATH="$AZURE_FILE_PATH"
if [[ ! -f "$SOURCE_PATH" ]]; then
    echo "Error: File $AZURE_FILE_PATH not found in Azure repo."
    exit 1
fi

git config --global user.email "$GITHUB_EMAIL"
git config --global user.name "$GITHUB_USER"
git config --global init.defaultBranch main

echo "Cloning GitHub repository..."
git clone https://$GITHUB_USER:$GITHUB_PAT@github.com/$AZURE_ORG/"$GITHUB_REPO".git "$GITHUB_LOCAL_DIR"

DEST_PATH="$GITHUB_LOCAL_DIR/specs/$SPEC_FILE"
cp "$SOURCE_PATH" "$DEST_PATH"

cd "$GITHUB_LOCAL_DIR" || exit
git add "specs/$SPEC_FILE"
git commit -m "Update $SERVICE_NAME spec" || {
      echo "No changes to commit."
      exit 1
    }
git push -f https://$GITHUB_USER:$GITHUB_PAT@github.com/$AZURE_ORG/$GITHUB_REPO.git $GITHUB_BRANCH
