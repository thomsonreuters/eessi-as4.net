import { SendingProcessingMode } from './SendingProcessingMode';
import { ReceivingProcessingMode } from './ReceivingProcessingMode';

export interface IPmode {
    name: string;
    type: number;
    hash: string;
    pmode: ReceivingProcessingMode | SendingProcessingMode;
}
