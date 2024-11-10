import { fetchWrapper } from './fetchWrapper';

export class FetchWrapperBuilder {
    private codesWithHandlers: {code: number, handler: any} [] = [];
    private minRequestTimeMs: number | undefined = undefined;
    private deserializeResponse: boolean = true;
    private timeout?: number;

    build = () => {
        const minRequestTimeMs = this.minRequestTimeMs;
        const deserializeResponse = this.deserializeResponse;
        const timeoutMs = this.timeout;
        const codesWithHandlers = this.codesWithHandlers;
        const result = <T>(path: string, options?: RequestInit) => fetchWrapper<T>(path, { ...options, codesWithHandlers, minRequestTimeMs, deserializeResponse, timeoutMs});
        return result;
    };
    customErrorHandling = (codes: number[], handler: (response: Response) => any) => {
        this.codesWithHandlers = codes.map(x => ({code: x, handler}));
        return this;
    };
    useMinRequestTime = (minTimeMs: number) => {
        this.minRequestTimeMs = minTimeMs;
        return this;
    };
    useTimeout = (timeoutMs: number) => {
        this.timeout = timeoutMs;
        return this;
    };
    setDoNotDeserializeResponse = () => {
        this.deserializeResponse = false;
        return this;
    };
}
