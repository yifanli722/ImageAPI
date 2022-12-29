# ImageAPI #
Dotnet API for handing image upload and retrieval. Demoing minimal API.

## Endpoints ##
`/api/ImageUpload`
<br>Upload an image, returns the images sha256 for later retrieval

`/api/RetrieveImage/{image_hash}`
<br>Retrieves a previously uploaded image

## Run Locally ##
Navigate into ImageApi folder, Start Postgres, Start API
```shell
cd ImageApi
docker-compose up
dotnet build
dotnet run
```
