/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
import { createGetObjectInstancesResultFromDiscriminatorValue, createProblemDetailsFromDiscriminatorValue, deserializeIntoProblemDetails, Generation, ObjectGCStatus, serializeProblemDetails, type GetObjectInstancesResult, type ProblemDetails } from '../../../../models/';
import { BaseRequestBuilder, HttpMethod, RequestInformation, type Parsable, type ParsableFactory, type RequestAdapter, type RequestConfiguration, type RequestOption } from '@microsoft/kiota-abstractions';

export interface WithMtItemRequestBuilderGetQueryParameters {
    gcStatus?: ObjectGCStatus;
    generation?: Generation;
}
/**
 * Builds and executes requests for operations under /api/dump/object-instances/{mt}
 */
export class WithMtItemRequestBuilder extends BaseRequestBuilder<WithMtItemRequestBuilder> {
    /**
     * Instantiates a new WithMtItemRequestBuilder and sets the default values.
     * @param pathParameters The raw url or the Url template parameters for the request.
     * @param requestAdapter The request adapter to use to execute the requests.
     */
    public constructor(pathParameters: Record<string, unknown> | string | undefined, requestAdapter: RequestAdapter) {
        super(pathParameters, requestAdapter, "{+baseurl}/api/dump/object-instances/{mt}{?gcStatus*,generation*}", (x, y) => new WithMtItemRequestBuilder(x, y));
    }
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a Promise of GetObjectInstancesResult
     */
    public get(requestConfiguration?: RequestConfiguration<WithMtItemRequestBuilderGetQueryParameters> | undefined) : Promise<GetObjectInstancesResult | undefined> {
        const requestInfo = this.toGetRequestInformation(
            requestConfiguration
        );
        const errorMapping = {
            "500": createProblemDetailsFromDiscriminatorValue,
        } as Record<string, ParsableFactory<Parsable>>;
        return this.requestAdapter.sendAsync<GetObjectInstancesResult>(requestInfo, createGetObjectInstancesResultFromDiscriminatorValue, errorMapping);
    }
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a RequestInformation
     */
    public toGetRequestInformation(requestConfiguration?: RequestConfiguration<WithMtItemRequestBuilderGetQueryParameters> | undefined) : RequestInformation {
        const requestInfo = new RequestInformation(HttpMethod.GET, this.urlTemplate, this.pathParameters);
        requestInfo.configure(requestConfiguration);
        requestInfo.headers.tryAdd("Accept", "application/json");
        return requestInfo;
    }
}
/* tslint:enable */
/* eslint-enable */
