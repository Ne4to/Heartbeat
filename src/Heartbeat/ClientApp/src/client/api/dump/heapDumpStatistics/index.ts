/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
import { createObjectTypeStatisticsFromDiscriminatorValue, createProblemDetailsFromDiscriminatorValue, deserializeIntoProblemDetails, Generation, ObjectGCStatus, serializeProblemDetails, type ObjectTypeStatistics, type ProblemDetails } from '../../../models/';
import { BaseRequestBuilder, HttpMethod, RequestInformation, type Parsable, type ParsableFactory, type RequestAdapter, type RequestConfiguration, type RequestOption } from '@microsoft/kiota-abstractions';

export interface HeapDumpStatisticsRequestBuilderGetQueryParameters {
    gcStatus?: ObjectGCStatus;
    generation?: Generation;
}
/**
 * Builds and executes requests for operations under /api/dump/heap-dump-statistics
 */
export class HeapDumpStatisticsRequestBuilder extends BaseRequestBuilder<HeapDumpStatisticsRequestBuilder> {
    /**
     * Instantiates a new HeapDumpStatisticsRequestBuilder and sets the default values.
     * @param pathParameters The raw url or the Url template parameters for the request.
     * @param requestAdapter The request adapter to use to execute the requests.
     */
    public constructor(pathParameters: Record<string, unknown> | string | undefined, requestAdapter: RequestAdapter) {
        super(pathParameters, requestAdapter, "{+baseurl}/api/dump/heap-dump-statistics{?gcStatus*,generation*}", (x, y) => new HeapDumpStatisticsRequestBuilder(x, y));
    }
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a Promise of ObjectTypeStatistics
     */
    public get(requestConfiguration?: RequestConfiguration<HeapDumpStatisticsRequestBuilderGetQueryParameters> | undefined) : Promise<ObjectTypeStatistics[] | undefined> {
        const requestInfo = this.toGetRequestInformation(
            requestConfiguration
        );
        const errorMapping = {
            "500": createProblemDetailsFromDiscriminatorValue,
        } as Record<string, ParsableFactory<Parsable>>;
        return this.requestAdapter.sendCollectionAsync<ObjectTypeStatistics>(requestInfo, createObjectTypeStatisticsFromDiscriminatorValue, errorMapping);
    }
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns a RequestInformation
     */
    public toGetRequestInformation(requestConfiguration?: RequestConfiguration<HeapDumpStatisticsRequestBuilderGetQueryParameters> | undefined) : RequestInformation {
        const requestInfo = new RequestInformation(HttpMethod.GET, this.urlTemplate, this.pathParameters);
        requestInfo.configure(requestConfiguration);
        requestInfo.headers.tryAdd("Accept", "application/json");
        return requestInfo;
    }
}
/* tslint:enable */
/* eslint-enable */
