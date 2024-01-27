import {useNotify} from "react-admin";

export const useNotifyError = () => {
    const notify = useNotify()

    const notifyError = (message: string) => {
        notify(message, {
            type: 'error',
            anchorOrigin: {vertical: 'top', horizontal: 'right'}
        })
    }

    return {
        notify,
        notifyError,
    }
}