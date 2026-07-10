import NextAuth from "next-auth";
import Keycloak from "next-auth/providers/keycloak";

export const { handlers, auth, signIn, signOut } = NextAuth({
  trustHost: true,
  secret: process.env.AUTH_SECRET ?? process.env.NEXTAUTH_SECRET,
  providers: [
    Keycloak({
      clientId: process.env.KEYCLOAK_CLIENT_ID!,
      clientSecret: process.env.KEYCLOAK_CLIENT_SECRET ?? "",
      issuer: process.env.KEYCLOAK_ISSUER!,
    }),
  ],
  callbacks: {
    async jwt({ token, account, profile }) {
      if (account) {
        token.accessToken  = account.access_token;
        token.idToken      = account.id_token;
        token.refreshToken = account.refresh_token;
        const realmAccess  = (profile as any)?.realm_access;
        token.roles        = realmAccess?.roles ?? [];
      }
      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken as string;
      session.idToken     = token.idToken     as string;
      session.roles       = (token.roles as string[]) ?? [];
      return session;
    },
  },
  events: {
    // Logout dari Keycloak saat signOut
    async signOut(message) {
      const idToken = (message as any)?.token?.idToken;
      if (idToken && process.env.KEYCLOAK_ISSUER) {
        const logoutUrl =
          `${process.env.KEYCLOAK_ISSUER}/protocol/openid-connect/logout` +
          `?id_token_hint=${idToken}` +
          `&post_logout_redirect_uri=${encodeURIComponent(process.env.NEXTAUTH_URL ?? "http://localhost:3000")}`;
        // Fire-and-forget — Keycloak akan invalidate session
        await fetch(logoutUrl).catch(() => {});
      }
    },
  },
  pages: {
    signIn: "/login",
  },
});
