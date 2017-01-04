import { SendingProcessingMode } from './SendingProcessingMode';
import { ReceivingProcessingMode } from './ReceivingProcessingMode';

export interface IPmode {
    name: string;
    pmode: ReceivingProcessingMode | SendingProcessingMode;
}
