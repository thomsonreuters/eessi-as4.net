import { SendingProcessingMode } from './SendingProcessingMode';
import { ReceivingProcessingMode } from './ReceivingProcessingMode';

export interface IPmode {
    name: string;
    type: number;
    pmode: ReceivingProcessingMode | SendingProcessingMode;
}
