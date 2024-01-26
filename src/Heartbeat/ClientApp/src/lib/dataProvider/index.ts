import {
    CreateParams, CreateResult, DeleteManyParams, DeleteManyResult, DeleteParams, DeleteResult,
    GetListParams,
    GetListResult,
    GetManyParams,
    GetManyReferenceParams, GetManyReferenceResult, GetManyResult,
    GetOneParams, GetOneResult, HttpError, number, UpdateManyParams, UpdateManyResult, UpdateParams, UpdateResult
} from "react-admin";
import getClient from "../getClient";

export const dataProvider = {
    // get a list of records based on sort, filter, and pagination
    getList: async (resource: string, params: GetListParams): Promise<GetListResult> => {
        if (resource === 'modules')
        {
            const client = getClient();
            const result = await client.api.dump.modules.get() || []
            const data = result.map((m) => ({ id: m.address, ...m }))

            return { data, total: undefined };
        }
        throw new HttpError("getList not implemented", 500, null)
    },
    // get a single record by id
    getOne: async (resource: string, params: GetOneParams): Promise<GetOneResult> => {
        throw new HttpError("getOne not implemented", 500, null)
    },
    // get a list of records based on an array of ids
    getMany: async (resource: string, params: GetManyParams): Promise<GetManyResult> => {
        throw new HttpError("getMany not implemented", 500, null)
    },
    // get the records referenced to another record, e.g. comments for a post
    getManyReference: async (resource: string, params: GetManyReferenceParams): Promise<GetManyReferenceResult> => {
        throw new HttpError("getManyReference not implemented", 500, null)
    },
    // create a record
    create: async (resource: string, params: CreateParams): Promise<CreateResult> => {
        throw new HttpError("create not implemented", 500, null)
    },
    // update a record based on a patch
    update: async (resource: string, params: UpdateParams): Promise<UpdateResult> => {
        throw new HttpError("update not implemented", 500, null)
    },
    // update a list of records based on an array of ids and a common patch
    updateMany: async (resource: string, params: UpdateManyParams): Promise<UpdateManyResult> => {
        throw new HttpError("updateMany not implemented", 500, null)
    },
    // delete a record by id
    delete: async (resource: string, params: DeleteParams): Promise<DeleteResult> => {
        throw new HttpError("delete not implemented", 500, null)
    },
    // delete a list of records based on an array of ids
    deleteMany: async (resource: string, params: DeleteManyParams): Promise<DeleteManyResult> => {
        throw new HttpError("deleteMany not implemented", 500, null)
    },
}