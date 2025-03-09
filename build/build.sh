#!/bin/sh

# Environment variables:
# AZURE_CONTAINER_REGISTRY 

# RUN THIS SCRIPT FROM THE ROOT OF THE REPOSITORY
# ./build/build.sh

COMMIT_HASH=$(git rev-parse --short HEAD)

docker build \
	--build-arg COMMIT_HASH=$COMMIT_HASH \
	-t $(AZURE_CONTAINER_REGISTRY).azurecr.io/catsudon-sheets:latest \
	-f "src/CatsUdon.CharacterSheets.Web/Dockerfile" \
	.
az login
az acr login --name $(AZURE_CONTAINER_REGISTRY)
docker push $(AZURE_CONTAINER_REGISTRY).azurecr.io/catsudon-sheets:latest