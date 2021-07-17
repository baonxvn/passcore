export async function resolveAppSettings(): Promise<any> {
    const response = await fetch('api/password');
    // const response = await fetch('https://localhost:44352/api/password');

    if (!response || response.status !== 200) {
        throw new Error('Error fetching settings.');
    }

    const responseBody = await response.text();

    const data = responseBody ? JSON.parse(responseBody) : {};

    return { ...data };
}
