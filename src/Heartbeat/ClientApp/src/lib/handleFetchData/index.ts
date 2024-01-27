export default function fetchData<T>(load: () => Promise<T | undefined>,
                                     onSetData: (data: (T | undefined)) => void,
                                     onSetLoading: (loading: boolean) => void,
                                     notifyError: (message: string) => void) {
    onSetLoading(true)
    load()
        .then((data => {
            onSetData(data)
        }))
        .catch((error) => {
            onSetData(undefined)
            console.error(error)

            const getErrorMessage = () => {
                // ProblemDetails
                return error.detail ?? error.title
                    ?? error;
            }

            const errorMessage = getErrorMessage()
            console.error(getErrorMessage())
            notifyError(errorMessage)
        })
        .finally(() => {
            onSetLoading(false)
        })
}