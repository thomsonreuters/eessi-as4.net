import { Receiver } from "./Receiver"
import { Transformer } from "./Transformer"
import { Steps } from "./Steps"
import { Decorator } from "./Decorator"

export class SettingsAgent {
		name: string;

		receiver: Receiver;
		transformer: Transformer;
		steps: Steps;
		decorator: Decorator;
}