/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
import { WithAddressItemRequestBuilder } from './item/';
import { BaseRequestBuilder, getPathParameters, type RequestAdapter } from '@microsoft/kiota-abstractions';

/**
 * Builds and executes requests for operations under /api/dump/object
 */
export class ObjectRequestBuilder extends BaseRequestBuilder<ObjectRequestBuilder> {
    /**
     * Gets an item from the ApiSdk.api.dump.object.item collection
     * @param address Unique identifier of the item
     * @returns a WithAddressItemRequestBuilder
     */
    public byAddress(address: number) : WithAddressItemRequestBuilder {
        if(!address) throw new Error("address cannot be undefined");
        const urlTplParams = getPathParameters(this.pathParameters);
        urlTplParams["address"] = address
        return new WithAddressItemRequestBuilder(urlTplParams, this.requestAdapter);
    }
    /**
     * Instantiates a new ObjectRequestBuilder and sets the default values.
     * @param pathParameters The raw url or the Url template parameters for the request.
     * @param requestAdapter The request adapter to use to execute the requests.
     */
    public constructor(pathParameters: Record<string, unknown> | string | undefined, requestAdapter: RequestAdapter) {
        super(pathParameters, requestAdapter, "{+baseurl}/api/dump/object", (x, y) => new ObjectRequestBuilder(x, y));
    }
}
/* tslint:enable */
/* eslint-enable */
