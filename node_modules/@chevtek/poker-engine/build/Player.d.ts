import { Card, Table } from ".";
export declare class Player {
    id: string;
    stackSize: number;
    table: Table;
    bet: number;
    raise?: number;
    holeCards?: [Card, Card];
    folded: boolean;
    showCards: boolean;
    left: boolean;
    constructor(id: string, stackSize: number, table: Table);
    get hand(): any;
    betAction(amount: number): void;
    callAction(): void;
    raiseAction(amount: number): void;
    checkAction(): void;
    foldAction(): void;
    legalActions(): string[];
}
//# sourceMappingURL=Player.d.ts.map