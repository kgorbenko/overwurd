import React from 'react';

export function useLocalStorage<TValue, TValueRaw>(key: string, empty: TValue, mappingFunc: (raw: TValueRaw) => TValue): [TValue, (value: TValue) => void] {
    const [storedValue, setStoredValue] = React.useState<TValue>(() => {
        const item = window.localStorage.getItem(key);

        return item !== null
            ? mappingFunc(JSON.parse(item))
            : empty;
    });

    const setValue = (value: TValue) => {
        setStoredValue(value);
        window.localStorage.setItem(key, JSON.stringify(value));
    };

    return [storedValue, setValue];
}