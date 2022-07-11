import React from 'react';
import dayjs, { Dayjs } from 'dayjs';

export function useCurrentDateTime(poolTimeout: number) {
    const [now, setNow] = React.useState<Dayjs>(dayjs.utc());

    React.useEffect(() => {
        const interval = setInterval(() => {
            setNow(dayjs.utc());
        }, poolTimeout);

        return () => clearInterval(interval);
    });

    return now;
}