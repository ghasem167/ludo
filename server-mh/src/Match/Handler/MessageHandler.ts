import { ServerOpCode } from "./Enums";

export class MessageHandler {
        SendToAllPlayers(dispatcher: nkruntime.MatchDispatcher,opcode:ServerOpCode, message: string): void {
            dispatcher.broadcastMessage(opcode, message);
        }


    }