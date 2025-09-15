# Backend Configuration Template

Copy this file to `appsettings.json` and customize as needed for your environment.

## For Development:
- Uses localhost CORS origins for frontend development
- Safe defaults for local development
- Copy as-is for most development scenarios

## For Production:
- Update `AllowedOrigins` to your actual domain(s)
- Consider stricter security headers
- Review all security settings
- Use environment-specific overrides (appsettings.Production.json)

## Security Note:
Never commit actual appsettings.json files to version control.
Use environment-specific files and secure configuration providers in production.
