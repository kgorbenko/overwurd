export function getModelErrors(modelState: object): string[] {
    const results = Object.keys(modelState);
    return results.map(x => modelState[x as keyof object]).flat();
}

export async function withLoadingAsync(setLoading: (value: boolean) => void, actionAsync: () => Promise<void>) {
    await withEnableAsync(setLoading, actionAsync);
}

export async function withEnableAsync(setter: (value: boolean) => void, actionAsync: () => Promise<void>) {
    try {
        setter(true);
        await actionAsync();
        setter(false);
    } finally {
        setter(false);
    }
}

export async function withDisableAsync(setter: (value: boolean) => void, actionAsync: () => Promise<void>) {
    try {
        setter(false);
        await actionAsync();
        setter(true);
    } finally {
        setter(true);
    }
}