/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
import { createProblemDetailsFromDiscriminatorValue, createStringInfoFromDiscriminatorValue, deserializeIntoProblemDetails, Generation, serializeProblemDetails, TraversingHeapModes, type ProblemDetails, type StringInfo } from '../../../models/';
import { BaseRequestBuilder, HttpMethod, RequestInformation, type Parsable, type ParsableFactory, type RequestAdapter, type RequestConfiguration, type RequestOption } from '@microsoft/kiota-abstractions';

export interface StringsRequestBuilderGetQueryParameters {
    generation?: Generation;
    traversingMode?: TraversingHeapModes;
}
/**
 * Builds and executes requests for operations under /api/dump/strings
 */
export class StringsRequestBuilder extends BaseRequestBuilder<StringsRequestBuilder> {
    /**
     * Instantiates a new StringsRequestBuilder and sets the default values.
     * @param pathParameters The raw url or the Url template parameters for the request.
     * @param requestAdapter The request adapter to use to execute the requests.
     */
    public constructor(pathParameters: Record<string, unknown> | string | undefined, requestAdapter: RequestAdapter) {
        super(pathParameters, requestAdapter, "{+baseurl}/api/dump/strings{?traversingMode*,generation*}", (x, y) => new StringsRequestBuilder(x, y));
    }
    /**
     * Get heap dump statistics
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a Promise of StringInfo
     */
    public get(requestConfiguration?: RequestConfiguration<StringsRequestBuilderGetQueryParameters> | undefined) : Promise<StringInfo[] | undefined> {
        const requestInfo = this.toGetRequestInformation(
            requestConfiguration
        );
        const errorMapping = {
            "500": createProblemDetailsFromDiscriminatorValue,
        } as Record<string, ParsableFactory<Parsable>>;
        return this.requestAdapter.sendCollectionAsync<StringInfo>(requestInfo, createStringInfoFromDiscriminatorValue, errorMapping);
    }
    /**
     * Get heap dump statistics
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a RequestInformation
     */
    public toGetRequestInformation(requestConfiguration?: RequestConfiguration<StringsRequestBuilderGetQueryParameters> | undefined) : RequestInformation {
        const requestInfo = new RequestInformation(HttpMethod.GET, this.urlTemplate, this.pathParameters);
        requestInfo.configure(requestConfiguration);
        requestInfo.headers.tryAdd("Accept", "application/json");
        return requestInfo;
    }
}
/* tslint:enable */
/* eslint-enable */
