export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000/api',
  authConfig: {
    authority: 'https://login.microsoftonline.com/{tenant-id}',
    clientId: '{client-id}',
    redirectUri: 'http://localhost:4200',
  },
  sessionTimeoutMinutes: 30,
};
