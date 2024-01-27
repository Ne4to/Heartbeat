import React from "react";
import Box from "@mui/material/Box";
import LinearProgress from "@mui/material/LinearProgress";
import ErrorIcon from "@mui/icons-material/ErrorOutlineOutlined";

export type ProgressContainerProps = {
    isLoading: boolean,
    children?: JSX.Element,
}

export const ProgressContainer = (props: ProgressContainerProps) => {
    if (props.isLoading)
        return (
            <Box sx={{width: '100%'}}>
                <LinearProgress color="primary"/>
            </Box>
        );

    // TODO add error message and remove notify
    // if (hasError || data === undefined)
    //     return <ErrorIcon/>

    return props.children ?? (<p>No data to display</p>);
}