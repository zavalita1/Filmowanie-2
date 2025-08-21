export const commonOnQueryStarted = async (
    setLoading: (loading: boolean) => void,
    promise : Promise<unknown>,
    showLoading = false,
    showSuccess = false,
    showError = false,
) => {
    try {
        /**
         * If we want to show our global loading state then we will dispatch an action to set the state to true once the
         * request starts
         */
        if (showLoading) {
            setLoading(true);
        }

        /**
         * wait for the request to be resolved
         */
        await promise;

        /**
         * If we want to show our global loading state then we will dispatch an action to set the state to false once the
         * request finishes
         */
        if (showLoading) {
            setLoading(false);
        }

        /**
         * Here you can also handle success requests to show the user a modal or toast with success message
         */
        if (showSuccess) {
            // Do something
        }
    } catch (err) {
        /**
         * If the request fails or something happens, then we will hide the loading state
         */
        if (showLoading) {
            setLoading(false);
        }

        /**
         * Here you can also handle errors to show a global modal or toast to let the user know what happened
         */
        if (showError) {
            // Do something
        }
    }
};