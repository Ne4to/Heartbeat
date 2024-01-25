/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
import { type AdditionalDataHolder, type ApiError, type Parsable, type ParseNode, type SerializationWriter } from '@microsoft/kiota-abstractions';

export type Architecture = (typeof ArchitectureObject)[keyof typeof ArchitectureObject];
export interface ArrayInfo extends Parsable {
    /**
     * The address property
     */
    address?: number;
    /**
     * The length property
     */
    length?: number;
    /**
     * The methodTable property
     */
    methodTable?: number;
    /**
     * The typeName property
     */
    typeName?: string;
    /**
     * The unusedItemsCount property
     */
    unusedItemsCount?: number;
    /**
     * The unusedPercent property
     */
    unusedPercent?: number;
    /**
     * The wasted property
     */
    wasted?: number;
}
export interface ClrObjectField extends Parsable {
    /**
     * The isValueType property
     */
    isValueType?: boolean;
    /**
     * The methodTable property
     */
    methodTable?: number;
    /**
     * The name property
     */
    name?: string;
    /**
     * The objectAddress property
     */
    objectAddress?: number;
    /**
     * The offset property
     */
    offset?: number;
    /**
     * The typeName property
     */
    typeName?: string;
    /**
     * The value property
     */
    value?: string;
}
export interface ClrObjectRootPath extends Parsable {
    /**
     * The pathItems property
     */
    pathItems?: RootPathItem[];
    /**
     * The root property
     */
    root?: RootInfo;
}
export type ClrRootKind = (typeof ClrRootKindObject)[keyof typeof ClrRootKindObject];
export function createArrayInfoFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoArrayInfo;
}
export function createClrObjectFieldFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoClrObjectField;
}
export function createClrObjectRootPathFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoClrObjectRootPath;
}
export function createDumpInfoFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoDumpInfo;
}
export function createGetClrObjectResultFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoGetClrObjectResult;
}
export function createGetObjectInstancesResultFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoGetObjectInstancesResult;
}
export function createHeapSegmentFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoHeapSegment;
}
export function createModuleFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoModule;
}
export function createObjectInstanceFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoObjectInstance;
}
export function createObjectTypeStatisticsFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoObjectTypeStatistics;
}
export function createProblemDetailsFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoProblemDetails;
}
export function createRootInfoFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoRootInfo;
}
export function createRootPathItemFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoRootPathItem;
}
export function createSparseArrayStatisticsFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoSparseArrayStatistics;
}
export function createStringDuplicateFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoStringDuplicate;
}
export function createStringInfoFromDiscriminatorValue(parseNode: ParseNode | undefined) {
    return deserializeIntoStringInfo;
}
export function deserializeIntoArrayInfo(arrayInfo: ArrayInfo | undefined = {} as ArrayInfo) : Record<string, (node: ParseNode) => void> {
    return {
        "address": n => { arrayInfo.address = n.getNumberValue(); },
        "length": n => { arrayInfo.length = n.getNumberValue(); },
        "methodTable": n => { arrayInfo.methodTable = n.getNumberValue(); },
        "typeName": n => { arrayInfo.typeName = n.getStringValue(); },
        "unusedItemsCount": n => { arrayInfo.unusedItemsCount = n.getNumberValue(); },
        "unusedPercent": n => { arrayInfo.unusedPercent = n.getNumberValue(); },
        "wasted": n => { arrayInfo.wasted = n.getNumberValue(); },
    }
}
export function deserializeIntoClrObjectField(clrObjectField: ClrObjectField | undefined = {} as ClrObjectField) : Record<string, (node: ParseNode) => void> {
    return {
        "isValueType": n => { clrObjectField.isValueType = n.getBooleanValue(); },
        "methodTable": n => { clrObjectField.methodTable = n.getNumberValue(); },
        "name": n => { clrObjectField.name = n.getStringValue(); },
        "objectAddress": n => { clrObjectField.objectAddress = n.getNumberValue(); },
        "offset": n => { clrObjectField.offset = n.getNumberValue(); },
        "typeName": n => { clrObjectField.typeName = n.getStringValue(); },
        "value": n => { clrObjectField.value = n.getStringValue(); },
    }
}
export function deserializeIntoClrObjectRootPath(clrObjectRootPath: ClrObjectRootPath | undefined = {} as ClrObjectRootPath) : Record<string, (node: ParseNode) => void> {
    return {
        "pathItems": n => { clrObjectRootPath.pathItems = n.getCollectionOfObjectValues<RootPathItem>(createRootPathItemFromDiscriminatorValue); },
        "root": n => { clrObjectRootPath.root = n.getObjectValue<RootInfo>(createRootInfoFromDiscriminatorValue); },
    }
}
export function deserializeIntoDumpInfo(dumpInfo: DumpInfo | undefined = {} as DumpInfo) : Record<string, (node: ParseNode) => void> {
    return {
        "architecture": n => { dumpInfo.architecture = n.getEnumValue<Architecture>(ArchitectureObject); },
        "canWalkHeap": n => { dumpInfo.canWalkHeap = n.getBooleanValue(); },
        "clrModulePath": n => { dumpInfo.clrModulePath = n.getStringValue(); },
        "dumpPath": n => { dumpInfo.dumpPath = n.getStringValue(); },
        "isServerHeap": n => { dumpInfo.isServerHeap = n.getBooleanValue(); },
        "platform": n => { dumpInfo.platform = n.getStringValue(); },
        "processId": n => { dumpInfo.processId = n.getNumberValue(); },
        "runtimeVersion": n => { dumpInfo.runtimeVersion = n.getStringValue(); },
    }
}
export function deserializeIntoGetClrObjectResult(getClrObjectResult: GetClrObjectResult | undefined = {} as GetClrObjectResult) : Record<string, (node: ParseNode) => void> {
    return {
        "address": n => { getClrObjectResult.address = n.getNumberValue(); },
        "fields": n => { getClrObjectResult.fields = n.getCollectionOfObjectValues<ClrObjectField>(createClrObjectFieldFromDiscriminatorValue); },
        "generation": n => { getClrObjectResult.generation = n.getEnumValue<Generation>(GenerationObject); },
        "methodTable": n => { getClrObjectResult.methodTable = n.getNumberValue(); },
        "moduleName": n => { getClrObjectResult.moduleName = n.getStringValue(); },
        "size": n => { getClrObjectResult.size = n.getNumberValue(); },
        "typeName": n => { getClrObjectResult.typeName = n.getStringValue(); },
        "value": n => { getClrObjectResult.value = n.getStringValue(); },
    }
}
export function deserializeIntoGetObjectInstancesResult(getObjectInstancesResult: GetObjectInstancesResult | undefined = {} as GetObjectInstancesResult) : Record<string, (node: ParseNode) => void> {
    return {
        "instances": n => { getObjectInstancesResult.instances = n.getCollectionOfObjectValues<ObjectInstance>(createObjectInstanceFromDiscriminatorValue); },
        "methodTable": n => { getObjectInstancesResult.methodTable = n.getNumberValue(); },
        "typeName": n => { getObjectInstancesResult.typeName = n.getStringValue(); },
    }
}
export function deserializeIntoHeapSegment(heapSegment: HeapSegment | undefined = {} as HeapSegment) : Record<string, (node: ParseNode) => void> {
    return {
        "end": n => { heapSegment.end = n.getNumberValue(); },
        "kind": n => { heapSegment.kind = n.getEnumValue<GCSegmentKind>(GCSegmentKindObject); },
        "size": n => { heapSegment.size = n.getNumberValue(); },
        "start": n => { heapSegment.start = n.getNumberValue(); },
    }
}
export function deserializeIntoModule(module: Module | undefined = {} as Module) : Record<string, (node: ParseNode) => void> {
    return {
        "address": n => { module.address = n.getNumberValue(); },
        "name": n => { module.name = n.getStringValue(); },
        "size": n => { module.size = n.getNumberValue(); },
    }
}
export function deserializeIntoObjectInstance(objectInstance: ObjectInstance | undefined = {} as ObjectInstance) : Record<string, (node: ParseNode) => void> {
    return {
        "address": n => { objectInstance.address = n.getNumberValue(); },
        "size": n => { objectInstance.size = n.getNumberValue(); },
    }
}
export function deserializeIntoObjectTypeStatistics(objectTypeStatistics: ObjectTypeStatistics | undefined = {} as ObjectTypeStatistics) : Record<string, (node: ParseNode) => void> {
    return {
        "instanceCount": n => { objectTypeStatistics.instanceCount = n.getNumberValue(); },
        "methodTable": n => { objectTypeStatistics.methodTable = n.getNumberValue(); },
        "totalSize": n => { objectTypeStatistics.totalSize = n.getNumberValue(); },
        "typeName": n => { objectTypeStatistics.typeName = n.getStringValue(); },
    }
}
export function deserializeIntoProblemDetails(problemDetails: ProblemDetails | undefined = {} as ProblemDetails) : Record<string, (node: ParseNode) => void> {
    return {
        "detail": n => { problemDetails.detail = n.getStringValue(); },
        "instance": n => { problemDetails.instance = n.getStringValue(); },
        "status": n => { problemDetails.status = n.getNumberValue(); },
        "title": n => { problemDetails.title = n.getStringValue(); },
        "type": n => { problemDetails.type = n.getStringValue(); },
    }
}
export function deserializeIntoRootInfo(rootInfo: RootInfo | undefined = {} as RootInfo) : Record<string, (node: ParseNode) => void> {
    return {
        "address": n => { rootInfo.address = n.getNumberValue(); },
        "isPinned": n => { rootInfo.isPinned = n.getBooleanValue(); },
        "kind": n => { rootInfo.kind = n.getEnumValue<ClrRootKind>(ClrRootKindObject); },
        "methodTable": n => { rootInfo.methodTable = n.getNumberValue(); },
        "size": n => { rootInfo.size = n.getNumberValue(); },
        "typeName": n => { rootInfo.typeName = n.getStringValue(); },
    }
}
export function deserializeIntoRootPathItem(rootPathItem: RootPathItem | undefined = {} as RootPathItem) : Record<string, (node: ParseNode) => void> {
    return {
        "address": n => { rootPathItem.address = n.getNumberValue(); },
        "generation": n => { rootPathItem.generation = n.getEnumValue<Generation>(GenerationObject); },
        "methodTable": n => { rootPathItem.methodTable = n.getNumberValue(); },
        "size": n => { rootPathItem.size = n.getNumberValue(); },
        "typeName": n => { rootPathItem.typeName = n.getStringValue(); },
    }
}
export function deserializeIntoSparseArrayStatistics(sparseArrayStatistics: SparseArrayStatistics | undefined = {} as SparseArrayStatistics) : Record<string, (node: ParseNode) => void> {
    return {
        "count": n => { sparseArrayStatistics.count = n.getNumberValue(); },
        "methodTable": n => { sparseArrayStatistics.methodTable = n.getNumberValue(); },
        "totalWasted": n => { sparseArrayStatistics.totalWasted = n.getNumberValue(); },
        "typeName": n => { sparseArrayStatistics.typeName = n.getStringValue(); },
    }
}
export function deserializeIntoStringDuplicate(stringDuplicate: StringDuplicate | undefined = {} as StringDuplicate) : Record<string, (node: ParseNode) => void> {
    return {
        "count": n => { stringDuplicate.count = n.getNumberValue(); },
        "fullLength": n => { stringDuplicate.fullLength = n.getNumberValue(); },
        "value": n => { stringDuplicate.value = n.getStringValue(); },
        "wastedMemory": n => { stringDuplicate.wastedMemory = n.getNumberValue(); },
    }
}
export function deserializeIntoStringInfo(stringInfo: StringInfo | undefined = {} as StringInfo) : Record<string, (node: ParseNode) => void> {
    return {
        "address": n => { stringInfo.address = n.getNumberValue(); },
        "length": n => { stringInfo.length = n.getNumberValue(); },
        "size": n => { stringInfo.size = n.getNumberValue(); },
        "value": n => { stringInfo.value = n.getStringValue(); },
    }
}
export interface DumpInfo extends Parsable {
    /**
     * The architecture property
     */
    architecture?: Architecture;
    /**
     * The canWalkHeap property
     */
    canWalkHeap?: boolean;
    /**
     * The clrModulePath property
     */
    clrModulePath?: string;
    /**
     * The dumpPath property
     */
    dumpPath?: string;
    /**
     * The isServerHeap property
     */
    isServerHeap?: boolean;
    /**
     * The platform property
     */
    platform?: string;
    /**
     * The processId property
     */
    processId?: number;
    /**
     * The runtimeVersion property
     */
    runtimeVersion?: string;
}
export type GCSegmentKind = (typeof GCSegmentKindObject)[keyof typeof GCSegmentKindObject];
export type Generation = (typeof GenerationObject)[keyof typeof GenerationObject];
export interface GetClrObjectResult extends Parsable {
    /**
     * The address property
     */
    address?: number;
    /**
     * The fields property
     */
    fields?: ClrObjectField[];
    /**
     * The generation property
     */
    generation?: Generation;
    /**
     * The methodTable property
     */
    methodTable?: number;
    /**
     * The moduleName property
     */
    moduleName?: string;
    /**
     * The size property
     */
    size?: number;
    /**
     * The typeName property
     */
    typeName?: string;
    /**
     * The value property
     */
    value?: string;
}
export interface GetObjectInstancesResult extends Parsable {
    /**
     * The instances property
     */
    instances?: ObjectInstance[];
    /**
     * The methodTable property
     */
    methodTable?: number;
    /**
     * The typeName property
     */
    typeName?: string;
}
export interface HeapSegment extends Parsable {
    /**
     * The end property
     */
    end?: number;
    /**
     * The kind property
     */
    kind?: GCSegmentKind;
    /**
     * The size property
     */
    size?: number;
    /**
     * The start property
     */
    start?: number;
}
export interface Module extends Parsable {
    /**
     * The address property
     */
    address?: number;
    /**
     * The name property
     */
    name?: string;
    /**
     * The size property
     */
    size?: number;
}
export interface ObjectInstance extends Parsable {
    /**
     * The address property
     */
    address?: number;
    /**
     * The size property
     */
    size?: number;
}
export interface ObjectTypeStatistics extends Parsable {
    /**
     * The instanceCount property
     */
    instanceCount?: number;
    /**
     * The methodTable property
     */
    methodTable?: number;
    /**
     * The totalSize property
     */
    totalSize?: number;
    /**
     * The typeName property
     */
    typeName?: string;
}
export interface ProblemDetails extends AdditionalDataHolder, ApiError, Parsable {
    /**
     * Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.
     */
    additionalData?: Record<string, unknown>;
    /**
     * The detail property
     */
    detail?: string;
    /**
     * The instance property
     */
    instance?: string;
    /**
     * The status property
     */
    status?: number;
    /**
     * The title property
     */
    title?: string;
    /**
     * The type property
     */
    type?: string;
}
export interface RootInfo extends Parsable {
    /**
     * The address property
     */
    address?: number;
    /**
     * The isPinned property
     */
    isPinned?: boolean;
    /**
     * The kind property
     */
    kind?: ClrRootKind;
    /**
     * The methodTable property
     */
    methodTable?: number;
    /**
     * The size property
     */
    size?: number;
    /**
     * The typeName property
     */
    typeName?: string;
}
export interface RootPathItem extends Parsable {
    /**
     * The address property
     */
    address?: number;
    /**
     * The generation property
     */
    generation?: Generation;
    /**
     * The methodTable property
     */
    methodTable?: number;
    /**
     * The size property
     */
    size?: number;
    /**
     * The typeName property
     */
    typeName?: string;
}
export function serializeArrayInfo(writer: SerializationWriter, arrayInfo: ArrayInfo | undefined = {} as ArrayInfo) : void {
    writer.writeNumberValue("address", arrayInfo.address);
    writer.writeNumberValue("length", arrayInfo.length);
    writer.writeNumberValue("methodTable", arrayInfo.methodTable);
    writer.writeStringValue("typeName", arrayInfo.typeName);
    writer.writeNumberValue("unusedItemsCount", arrayInfo.unusedItemsCount);
    writer.writeNumberValue("unusedPercent", arrayInfo.unusedPercent);
    writer.writeNumberValue("wasted", arrayInfo.wasted);
}
export function serializeClrObjectField(writer: SerializationWriter, clrObjectField: ClrObjectField | undefined = {} as ClrObjectField) : void {
    writer.writeBooleanValue("isValueType", clrObjectField.isValueType);
    writer.writeNumberValue("methodTable", clrObjectField.methodTable);
    writer.writeStringValue("name", clrObjectField.name);
    writer.writeNumberValue("objectAddress", clrObjectField.objectAddress);
    writer.writeNumberValue("offset", clrObjectField.offset);
    writer.writeStringValue("typeName", clrObjectField.typeName);
    writer.writeStringValue("value", clrObjectField.value);
}
export function serializeClrObjectRootPath(writer: SerializationWriter, clrObjectRootPath: ClrObjectRootPath | undefined = {} as ClrObjectRootPath) : void {
    writer.writeCollectionOfObjectValues<RootPathItem>("pathItems", clrObjectRootPath.pathItems, serializeRootPathItem);
    writer.writeObjectValue<RootInfo>("root", clrObjectRootPath.root, serializeRootInfo);
}
export function serializeDumpInfo(writer: SerializationWriter, dumpInfo: DumpInfo | undefined = {} as DumpInfo) : void {
    writer.writeEnumValue<Architecture>("architecture", dumpInfo.architecture);
    writer.writeBooleanValue("canWalkHeap", dumpInfo.canWalkHeap);
    writer.writeStringValue("clrModulePath", dumpInfo.clrModulePath);
    writer.writeStringValue("dumpPath", dumpInfo.dumpPath);
    writer.writeBooleanValue("isServerHeap", dumpInfo.isServerHeap);
    writer.writeStringValue("platform", dumpInfo.platform);
    writer.writeNumberValue("processId", dumpInfo.processId);
    writer.writeStringValue("runtimeVersion", dumpInfo.runtimeVersion);
}
export function serializeGetClrObjectResult(writer: SerializationWriter, getClrObjectResult: GetClrObjectResult | undefined = {} as GetClrObjectResult) : void {
    writer.writeNumberValue("address", getClrObjectResult.address);
    writer.writeCollectionOfObjectValues<ClrObjectField>("fields", getClrObjectResult.fields, serializeClrObjectField);
    writer.writeEnumValue<Generation>("generation", getClrObjectResult.generation);
    writer.writeNumberValue("methodTable", getClrObjectResult.methodTable);
    writer.writeStringValue("moduleName", getClrObjectResult.moduleName);
    writer.writeNumberValue("size", getClrObjectResult.size);
    writer.writeStringValue("typeName", getClrObjectResult.typeName);
    writer.writeStringValue("value", getClrObjectResult.value);
}
export function serializeGetObjectInstancesResult(writer: SerializationWriter, getObjectInstancesResult: GetObjectInstancesResult | undefined = {} as GetObjectInstancesResult) : void {
    writer.writeCollectionOfObjectValues<ObjectInstance>("instances", getObjectInstancesResult.instances, serializeObjectInstance);
    writer.writeNumberValue("methodTable", getObjectInstancesResult.methodTable);
    writer.writeStringValue("typeName", getObjectInstancesResult.typeName);
}
export function serializeHeapSegment(writer: SerializationWriter, heapSegment: HeapSegment | undefined = {} as HeapSegment) : void {
    writer.writeNumberValue("end", heapSegment.end);
    writer.writeEnumValue<GCSegmentKind>("kind", heapSegment.kind);
    writer.writeNumberValue("start", heapSegment.start);
}
export function serializeModule(writer: SerializationWriter, module: Module | undefined = {} as Module) : void {
    writer.writeNumberValue("address", module.address);
    writer.writeStringValue("name", module.name);
    writer.writeNumberValue("size", module.size);
}
export function serializeObjectInstance(writer: SerializationWriter, objectInstance: ObjectInstance | undefined = {} as ObjectInstance) : void {
    writer.writeNumberValue("address", objectInstance.address);
    writer.writeNumberValue("size", objectInstance.size);
}
export function serializeObjectTypeStatistics(writer: SerializationWriter, objectTypeStatistics: ObjectTypeStatistics | undefined = {} as ObjectTypeStatistics) : void {
    writer.writeNumberValue("instanceCount", objectTypeStatistics.instanceCount);
    writer.writeNumberValue("methodTable", objectTypeStatistics.methodTable);
    writer.writeNumberValue("totalSize", objectTypeStatistics.totalSize);
    writer.writeStringValue("typeName", objectTypeStatistics.typeName);
}
export function serializeProblemDetails(writer: SerializationWriter, problemDetails: ProblemDetails | undefined = {} as ProblemDetails) : void {
    writer.writeStringValue("detail", problemDetails.detail);
    writer.writeStringValue("instance", problemDetails.instance);
    writer.writeNumberValue("status", problemDetails.status);
    writer.writeStringValue("title", problemDetails.title);
    writer.writeStringValue("type", problemDetails.type);
    writer.writeAdditionalData(problemDetails.additionalData);
}
export function serializeRootInfo(writer: SerializationWriter, rootInfo: RootInfo | undefined = {} as RootInfo) : void {
    writer.writeNumberValue("address", rootInfo.address);
    writer.writeBooleanValue("isPinned", rootInfo.isPinned);
    writer.writeEnumValue<ClrRootKind>("kind", rootInfo.kind);
    writer.writeNumberValue("methodTable", rootInfo.methodTable);
    writer.writeNumberValue("size", rootInfo.size);
    writer.writeStringValue("typeName", rootInfo.typeName);
}
export function serializeRootPathItem(writer: SerializationWriter, rootPathItem: RootPathItem | undefined = {} as RootPathItem) : void {
    writer.writeNumberValue("address", rootPathItem.address);
    writer.writeEnumValue<Generation>("generation", rootPathItem.generation);
    writer.writeNumberValue("methodTable", rootPathItem.methodTable);
    writer.writeNumberValue("size", rootPathItem.size);
    writer.writeStringValue("typeName", rootPathItem.typeName);
}
export function serializeSparseArrayStatistics(writer: SerializationWriter, sparseArrayStatistics: SparseArrayStatistics | undefined = {} as SparseArrayStatistics) : void {
    writer.writeNumberValue("count", sparseArrayStatistics.count);
    writer.writeNumberValue("methodTable", sparseArrayStatistics.methodTable);
    writer.writeNumberValue("totalWasted", sparseArrayStatistics.totalWasted);
    writer.writeStringValue("typeName", sparseArrayStatistics.typeName);
}
export function serializeStringDuplicate(writer: SerializationWriter, stringDuplicate: StringDuplicate | undefined = {} as StringDuplicate) : void {
    writer.writeNumberValue("count", stringDuplicate.count);
    writer.writeNumberValue("fullLength", stringDuplicate.fullLength);
    writer.writeStringValue("value", stringDuplicate.value);
    writer.writeNumberValue("wastedMemory", stringDuplicate.wastedMemory);
}
export function serializeStringInfo(writer: SerializationWriter, stringInfo: StringInfo | undefined = {} as StringInfo) : void {
    writer.writeNumberValue("address", stringInfo.address);
    writer.writeNumberValue("length", stringInfo.length);
    writer.writeNumberValue("size", stringInfo.size);
    writer.writeStringValue("value", stringInfo.value);
}
export interface SparseArrayStatistics extends Parsable {
    /**
     * The count property
     */
    count?: number;
    /**
     * The methodTable property
     */
    methodTable?: number;
    /**
     * The totalWasted property
     */
    totalWasted?: number;
    /**
     * The typeName property
     */
    typeName?: string;
}
export interface StringDuplicate extends Parsable {
    /**
     * The count property
     */
    count?: number;
    /**
     * The fullLength property
     */
    fullLength?: number;
    /**
     * The value property
     */
    value?: string;
    /**
     * The wastedMemory property
     */
    wastedMemory?: number;
}
export interface StringInfo extends Parsable {
    /**
     * The address property
     */
    address?: number;
    /**
     * The length property
     */
    length?: number;
    /**
     * The size property
     */
    size?: number;
    /**
     * The value property
     */
    value?: string;
}
export type TraversingHeapModes = (typeof TraversingHeapModesObject)[keyof typeof TraversingHeapModesObject];
export const ArchitectureObject = {
    X86: "X86",
    X64: "X64",
    Arm: "Arm",
    Arm64: "Arm64",
    Wasm: "Wasm",
    S390x: "S390x",
    LoongArch64: "LoongArch64",
    Armv6: "Armv6",
    Ppc64le: "Ppc64le",
}  as const;
export const ClrRootKindObject = {
    None: "None",
    FinalizerQueue: "FinalizerQueue",
    StrongHandle: "StrongHandle",
    PinnedHandle: "PinnedHandle",
    Stack: "Stack",
    RefCountedHandle: "RefCountedHandle",
    AsyncPinnedHandle: "AsyncPinnedHandle",
    SizedRefHandle: "SizedRefHandle",
}  as const;
export const GCSegmentKindObject = {
    Generation0: "Generation0",
    Generation1: "Generation1",
    Generation2: "Generation2",
    Large: "Large",
    Pinned: "Pinned",
    Frozen: "Frozen",
    Ephemeral: "Ephemeral",
}  as const;
export const GenerationObject = {
    Generation0: "Generation0",
    Generation1: "Generation1",
    Generation2: "Generation2",
    Large: "Large",
    Pinned: "Pinned",
    Frozen: "Frozen",
    Unknown: "Unknown",
}  as const;
export const TraversingHeapModesObject = {
    Live: "Live",
    Dead: "Dead",
    All: "All",
}  as const;
/* tslint:enable */
/* eslint-enable */
