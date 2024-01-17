/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
import { createGetClrObjectResultFromDiscriminatorValue, createProblemDetailsFromDiscriminatorValue, deserializeIntoProblemDetails, serializeProblemDetails, type GetClrObjectResult, type ProblemDetails } from '../../../../models/';
import { BaseRequestBuilder, HttpMethod, RequestInformation, type Parsable, type ParsableFactory, type RequestAdapter, type RequestConfiguration, type RequestOption } from '@microsoft/kiota-abstractions';

/**
 * Builds and executes requests for operations under /api/dump/object/{address}
 */
export class WithAddressItemRequestBuilder extends BaseRequestBuilder<WithAddressItemRequestBuilder> {
    /**
     * Instantiates a new WithAddressItemRequestBuilder and sets the default values.
     * @param pathParameters The raw url or the Url template parameters for the request.
     * @param requestAdapter The request adapter to use to execute the requests.
     */
    public constructor(pathParameters: Record<string, unknown> | string | undefined, requestAdapter: RequestAdapter) {
        super(pathParameters, requestAdapter, "{+baseurl}/api/dump/object/{address}", (x, y) => new WithAddressItemRequestBuilder(x, y));
    }
    /**
     * Get object
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a Promise of GetClrObjectResult
     */
    public get(requestConfiguration?: RequestConfiguration<object> | undefined) : Promise<GetClrObjectResult | undefined> {
        const requestInfo = this.toGetRequestInformation(
            requestConfiguration
        );
        const errorMapping = {
            "404": createProblemDetailsFromDiscriminatorValue,
        } as Record<string, ParsableFactory<Parsable>>;
        return this.requestAdapter.sendAsync<GetClrObjectResult>(requestInfo, createGetClrObjectResultFromDiscriminatorValue, errorMapping);
    }
    /**
     * Get object
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a RequestInformation
     */
    public toGetRequestInformation(requestConfiguration?: RequestConfiguration<object> | undefined) : RequestInformation {
        const requestInfo = new RequestInformation(HttpMethod.GET, this.urlTemplate, this.pathParameters);
        requestInfo.configure(requestConfiguration);
        requestInfo.headers.tryAdd("Accept", "application/json");
        return requestInfo;
    }
}
/* tslint:enable */
/* eslint-enable */
