import { Location } from 'react-router';
import { useLocation as nativeUseLocation } from 'react-router-dom';

export interface LocationWithState extends Location {
    state: { from?: LocationWithState } | undefined;
}

export const useLocation = (): LocationWithState => nativeUseLocation() as LocationWithState;