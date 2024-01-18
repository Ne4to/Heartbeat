/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
import { HeapDumpStatisticsRequestBuilder } from './heapDumpStatistics/';
import { ModulesRequestBuilder } from './modules/';
import { ObjectRequestBuilder } from './object/';
import { ObjectInstancesRequestBuilder } from './objectInstances/';
import { SegmentsRequestBuilder } from './segments/';
import { BaseRequestBuilder, type RequestAdapter } from '@microsoft/kiota-abstractions';

/**
 * Builds and executes requests for operations under /api/dump
 */
export class DumpRequestBuilder extends BaseRequestBuilder<DumpRequestBuilder> {
    /**
     * The heapDumpStatistics property
     */
    public get heapDumpStatistics(): HeapDumpStatisticsRequestBuilder {
        return new HeapDumpStatisticsRequestBuilder(this.pathParameters, this.requestAdapter);
    }
    /**
     * The modules property
     */
    public get modules(): ModulesRequestBuilder {
        return new ModulesRequestBuilder(this.pathParameters, this.requestAdapter);
    }
    /**
     * The object property
     */
    public get object(): ObjectRequestBuilder {
        return new ObjectRequestBuilder(this.pathParameters, this.requestAdapter);
    }
    /**
     * The objectInstances property
     */
    public get objectInstances(): ObjectInstancesRequestBuilder {
        return new ObjectInstancesRequestBuilder(this.pathParameters, this.requestAdapter);
    }
    /**
     * The segments property
     */
    public get segments(): SegmentsRequestBuilder {
        return new SegmentsRequestBuilder(this.pathParameters, this.requestAdapter);
    }
    /**
     * Instantiates a new DumpRequestBuilder and sets the default values.
     * @param pathParameters The raw url or the Url template parameters for the request.
     * @param requestAdapter The request adapter to use to execute the requests.
     */
    public constructor(pathParameters: Record<string, unknown> | string | undefined, requestAdapter: RequestAdapter) {
        super(pathParameters, requestAdapter, "{+baseurl}/api/dump", (x, y) => new DumpRequestBuilder(x, y));
    }
}
/* tslint:enable */
/* eslint-enable */
