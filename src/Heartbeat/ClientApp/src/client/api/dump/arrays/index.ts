/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
import { createArrayInfoFromDiscriminatorValue, createProblemDetailsFromDiscriminatorValue, deserializeIntoProblemDetails, Generation, serializeProblemDetails, TraversingHeapModes, type ArrayInfo, type ProblemDetails } from '../../../models/';
import { BaseRequestBuilder, HttpMethod, RequestInformation, type Parsable, type ParsableFactory, type RequestAdapter, type RequestConfiguration, type RequestOption } from '@microsoft/kiota-abstractions';

export interface ArraysRequestBuilderGetQueryParameters {
    generation?: Generation;
    traversingMode?: TraversingHeapModes;
}
/**
 * Builds and executes requests for operations under /api/dump/arrays
 */
export class ArraysRequestBuilder extends BaseRequestBuilder<ArraysRequestBuilder> {
    /**
     * Instantiates a new ArraysRequestBuilder and sets the default values.
     * @param pathParameters The raw url or the Url template parameters for the request.
     * @param requestAdapter The request adapter to use to execute the requests.
     */
    public constructor(pathParameters: Record<string, unknown> | string | undefined, requestAdapter: RequestAdapter) {
        super(pathParameters, requestAdapter, "{+baseurl}/api/dump/arrays{?traversingMode*,generation*}", (x, y) => new ArraysRequestBuilder(x, y));
    }
    /**
     * Get arrays
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a Promise of ArrayInfo
     */
    public get(requestConfiguration?: RequestConfiguration<ArraysRequestBuilderGetQueryParameters> | undefined) : Promise<ArrayInfo[] | undefined> {
        const requestInfo = this.toGetRequestInformation(
            requestConfiguration
        );
        const errorMapping = {
            "500": createProblemDetailsFromDiscriminatorValue,
        } as Record<string, ParsableFactory<Parsable>>;
        return this.requestAdapter.sendCollectionAsync<ArrayInfo>(requestInfo, createArrayInfoFromDiscriminatorValue, errorMapping);
    }
    /**
     * Get arrays
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a RequestInformation
     */
    public toGetRequestInformation(requestConfiguration?: RequestConfiguration<ArraysRequestBuilderGetQueryParameters> | undefined) : RequestInformation {
        const requestInfo = new RequestInformation(HttpMethod.GET, this.urlTemplate, this.pathParameters);
        requestInfo.configure(requestConfiguration);
        requestInfo.headers.tryAdd("Accept", "application/json");
        return requestInfo;
    }
}
/* tslint:enable */
/* eslint-enable */
