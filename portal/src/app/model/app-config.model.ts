export interface IAppConfig {
    env: {
        name: string;
    };
    ApiUrlBase: string;

    authentication: {
        clientID: string;
        authority: string;
    }
}