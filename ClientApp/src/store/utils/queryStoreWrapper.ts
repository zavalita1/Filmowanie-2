import { toast } from "sonner";

export const commonOnQueryStarted = async (
    setLoading: (loading: boolean) => void,
    promise : Promise<unknown>,
    showLoading = true,
    showSuccess: boolean | string = false,
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

        if (!!showSuccess) {
            const message = showSuccess !== true ? showSuccess : "Panie prezesie, melduję wykonanie zadania!";
            toast.info(message);
        }
    } catch (err) {
        if (showLoading) {
            setLoading(false);
        }

        if (err.error?.status === "TIMEOUT_ERROR") {
            toast.error("Przekroczono czas oczekiwania na odpowiedź serwera. Najpewniej microsoft odpierdala. Albo masz tragiczny net.", {
                 classNames: {
                    description: "!text-foreground/80",
                },
                className: "text-5xl",
                richColors: true
            });
            return;
        }

        if (additionalErrorHandlingCallback) {
            await additionalErrorHandlingCallback();
        }

        if (showError && err?.error?.status !== 401) {
            let errorMessage = err?.error?.message;
            if (!errorMessage) errorMessage = err?.error?.data;
            if (!errorMessage && typeof err?.error === "string") errorMessage = err?.error;
            if (!errorMessage) errorMessage = "Coś poszło nie tak :( ";

            toast.error(errorMessage, {
                classNames: {
                    description: "!text-foreground/80",
                },
                className: "text-5xl",
                richColors: true
            });
        }
    }
};