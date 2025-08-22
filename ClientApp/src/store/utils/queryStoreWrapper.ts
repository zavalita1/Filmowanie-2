export const commonOnQueryStarted = async (
    setLoading: (loading: boolean) => void,
    promise : Promise<unknown>,
    showLoading = false,
    showSuccess = false,
    showError = false,
) => {
    try {
        if (showLoading) {
            setLoading(true);
        }

        await promise;

        if (showLoading) {
            setLoading(false);
        }

        if (showSuccess) {
        }
    } catch (err) {
        if (showLoading) {
            setLoading(false);
        }

        if (showError) {
            // Do something
        }
    }
};