import { AnonymousAuthenticationProvider } from '@microsoft/kiota-abstractions';
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';
import { HeartbeatClient } from '../../client/heartbeatClient';

export default function getClient() : HeartbeatClient {
    // API requires no authentication, so use the anonymous
    // authentication provider
    const authProvider = new AnonymousAuthenticationProvider();
    // Create request adapter using the fetch-based implementation
    const adapter = new FetchRequestAdapter(authProvider);
    // Create the API client
    const client = new HeartbeatClient(adapter);

    return client;
}
