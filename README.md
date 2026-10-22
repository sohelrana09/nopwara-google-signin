# Google Sign-in for nopCommerce (Nopwara)

"Continue with Google" for nopCommerce 5.00. It uses the official `Microsoft.AspNetCore.Authentication.Google` handler (OAuth 2.0 / OpenID Connect, scopes `openid email profile`). Login, registration and account association are done by the core external authentication service, the same way as the bundled Facebook plugin.

## Features

- **Button:** "Continue with Google" on the login and registration pages, styled for Nopwara (Tailwind) and following Google's button guidelines.
- **Accounts:** core decides what happens.
  - It signs in a customer already associated with the Google account.
  - Otherwise it registers a new customer (an email that already exists is refused, not linked).
  - Customers can see and remove associations under My account → External authentication.
- **Account picker:** Google always shows the account chooser (`prompt=select_account`).
- **Errors:** a cancel or error returns to the login page instead of an exception page.
- **Configure page:** shows the exact *Authorized redirect URI* to copy into Google.

## Setup

1. Copy this folder to `src/Plugins/`, add the project to `NopCommerce.sln` and build. The NuGet handler is copied to the plugin folder (`CopyLocalLockFileAssemblies`).
2. Install **Google sign-in (Nopwara)** (Configuration → Local plugins) and restart.
3. In the [Google Cloud console](https://console.cloud.google.com/apis/credentials):
   1. Set up the OAuth consent screen.
   2. Create an **OAuth client ID** of type **Web application**.
   3. Add the redirect URI shown on the plugin configure page: `https://your-store/signin-google`.
4. Paste the client ID and secret into the configure page and save.
5. Activate it under Configuration → Authentication → External authentication.

For the Nopwara theme, add this line to the theme's Tailwind entry CSS, then rebuild the CSS:
`@source "../../../../../Plugins/Nop.Plugin.ExternalAuth.Google/Views";`

## License

Copyright (C) 2026 Sohel Rana

Licensed under the [GNU Affero General Public License v3.0](LICENSE), the license nopCommerce itself is built on (the [nopCommerce Public License](https://www.nopcommerce.com/license) is AGPL v3 plus its additional terms). Parts of this project are adapted from nopCommerce source code. A store that uses it must keep nopCommerce's "Powered by nopCommerce" link, as the nopCommerce Public License requires.
