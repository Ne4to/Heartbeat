import {Dispatch, SetStateAction, useState} from "react";

export const useStateWithLoading = <T>() : [T | undefined, Dispatch<SetStateAction<T | undefined>>, boolean, Dispatch<SetStateAction<boolean>>] => {
    const [data, setData] = useState<T>();
    const [isLoading, setIsLoading] = useState(false);

    return [
        data,
        setData,
        isLoading,
        setIsLoading
    ]
}