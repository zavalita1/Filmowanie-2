import { toast } from "sonner";

export const commonOnQueryStarted = async (
    setLoading: (loading: boolean) => void,
    promise : Promise<unknown>,
    showLoading = true,
    showSuccess = false,
    showError = true,
    additionalErrorHandlingCallback?: () => Promise<unknown>,
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
            toast.info("Panie prezesie, melduję wykonanie zadania!")
        }
    } catch (err) {
        if (showLoading) {
            setLoading(false);
        }

        if (additionalErrorHandlingCallback) {
            await additionalErrorHandlingCallback();
        }

        if (showError) {
            toast.error("Coś poszło nie tak :(", {
                classNames: {
                    description: "!text-foreground/80",
                },
                className: "text-5xl",
                richColors: true
            });
        }
    }
};