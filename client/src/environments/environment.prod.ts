export const environment = {
  production: true,
  apiBaseUrl: '/api',
  authConfig: {
    authority: 'https://login.microsoftonline.com/{tenant-id}',
    clientId: '{client-id}',
    redirectUri: 'https://candidacy-management.example.com',
  },
  sessionTimeoutMinutes: 30,
};
