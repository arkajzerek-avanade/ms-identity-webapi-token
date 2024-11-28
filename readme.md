### Back-End Token 

1. Open in the browser:

```yaml
https://login.microsoftonline.com/f0488bf5-fdc1-419b-aecf-f8e9e04c82e7/oauth2/v2.0/authorize?client_id=f0488bf5-fdc1-419b-aecf-f8e9e04c82e7&response_type=code&redirect_uri=https://localhost&response_mode=query&scope=api://TSG-ACCLETTER-dev-workflow.bainlab.com/workflow.UserAccess
```

this will redirect to such url after authentication:

```
https://localhost/?code=1.ARsA...&session_state=7811bc69-d9cf-4e5c-939a-9ca7f57497bd#
```

1. Get the code and param value and use in the following request (POST by curl)

```
curl -X POST https://login.microsoftonline.com/f0488bf5-fdc1-419b-aecf-f8e9e04c82e7/oauth2/v2.0/token \
-d 'client_id=47ae2cc2-2fb9-4cc9-9829-a19c6c1838d9' \
-d 'api://TSG-ACCLETTER-dev-workflow.bainlab.com/workflow.UserAccess' \
-d 'code=1.ARsA...' \
-d 'redirect_uri=https://localhost' \
-d 'grant_type=authorization_code' \
-d 'client_secret=VV...'
```

This will return

```
{
"token_type": "Bearer",
"scope": "api://TSG-ACCLETTER-dev-workflow.bainlab.com/workflow.UserAccess",
"expires_in": 4435,
"ext_expires_in": 4435,
"access_token": "eyJ0..."
}
```
