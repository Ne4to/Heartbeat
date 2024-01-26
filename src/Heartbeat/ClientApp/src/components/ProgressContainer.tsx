import React, {useEffect} from "react";
import {useNotify} from "react-admin";
import Box from "@mui/material/Box";
import LinearProgress from "@mui/material/LinearProgress";
import ErrorIcon from "@mui/icons-material/ErrorOutlineOutlined";

export type ProgressContainerProps<T> = {
    loadData: () => Promise<T>,
    getChildren: (data: T) => JSX.Element
}

export const ProgressContainer = (props: ProgressContainerProps<any>) => {
    const [loading, setLoading] = React.useState<boolean>(false)
    const [hasError, setHasError] = React.useState<boolean>(false)
    const [data, setData] = React.useState()
    const notify = useNotify();

    useEffect(() => {
        setLoading(true)
        setHasError(false)

        props.loadData()
            .then((data) => {
                setData(data)
            })
            .catch((error) => {
                setHasError(true)
                notify('API call error', {
                    type: 'error',
                    anchorOrigin: {vertical: 'top', horizontal: 'right'}
                })
            })
            .finally(() => {
                setLoading(false)
            })

    }, [props, notify]);

    if (loading)
        return (
            <Box sx={{width: '100%'}}>
                <LinearProgress color="primary"/>
            </Box>
        );

    // TODO add error message and remove notify
    if (hasError || data === undefined)
        return <ErrorIcon/>

    return props.getChildren(data);
}