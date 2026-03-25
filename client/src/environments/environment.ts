export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7001/api',
  authConfig: {
    authority: 'https://login.microsoftonline.com/{tenant-id}',
    clientId: '{client-id}',
    redirectUri: 'http://localhost:4200',
  },
  sessionTimeoutMinutes: 30,
};
