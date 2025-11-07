
export interface DashboardMetric {
    key: string;
    label: string;
    value: number | string;
    description?: string;
    status?: 'good' | 'warn' | 'bad';
    delta?: number; // positive -> up, negative -> down
    trend?: 'up' | 'down' | 'flat';
}
