type ApiResponse = {
    id: number;
    success: boolean;
    message: string;
    data?: Data
}

type Data = {
    username: string;
    readonlyUrl: string;
    name: string;
    baseHitPoints: number;
    temporaryHitPoints: number;
    stats: Stat[];
    race: Race;
}

type Stat = {
    id: number;
    value: number;
}

type Race = {
    baseRaceName: string;
}