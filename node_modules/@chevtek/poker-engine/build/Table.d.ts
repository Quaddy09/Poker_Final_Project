import { Card, Player } from ".";
export declare class Table {
    buyIn: number;
    smallBlind: number;
    bigBlind: number;
    autoMoveDealer: boolean;
    bigBlindPosition?: number;
    communityCards: Card[];
    currentBet?: number;
    currentPosition?: number;
    currentRound?: BettingRound;
    dealerPosition?: number;
    debug: boolean;
    deck: Card[];
    handNumber: number;
    lastPosition?: number;
    lastRaise?: number;
    players: (Player | null)[];
    pots: Pot[];
    smallBlindPosition?: number;
    winners?: Player[];
    constructor(buyIn?: number, smallBlind?: number, bigBlind?: number);
    get actingPlayers(): Player[];
    get activePlayers(): Player[];
    get bigBlindPlayer(): Player | null | undefined;
    get currentActor(): Player | null | undefined;
    get currentPot(): Pot;
    get dealer(): Player | null | undefined;
    get lastActor(): Player | null | undefined;
    get sidePots(): Pot[] | undefined;
    get smallBlindPlayer(): Player | null | undefined;
    moveDealer(seatNumber: number): void;
    sitDown(id: string, buyIn: number, seatNumber?: number): number;
    standUp(player: Player | string): Player[];
    cleanUp(): void;
    dealCards(): void;
    nextAction(): void;
    gatherBets(): void;
    nextRound(): void;
    showdown(): void;
    newDeck(): Card[];
}
export declare class Pot {
    amount: number;
    eligiblePlayers: Player[];
    winners?: Player[];
}
export declare enum BettingRound {
    PRE_FLOP = "pre-flop",
    FLOP = "flop",
    TURN = "turn",
    RIVER = "river"
}
//# sourceMappingURL=Table.d.ts.map