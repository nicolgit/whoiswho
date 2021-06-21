export interface IAppConfig {
    env: {
        name: string;
    };
    ApiUrlBase: string;
    ApiAccessScope: string;

    authentication: {
        clientID: string;
        authority: string;
    }

    MSALGuardScopes: string[];

    protectedResources: { endpoint: string, scopes: string[] }[];
}