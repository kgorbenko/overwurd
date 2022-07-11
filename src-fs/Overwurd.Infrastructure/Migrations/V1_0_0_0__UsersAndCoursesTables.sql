﻿create schema "overwurd"

create table "overwurd"."Users" (
    "Id" integer generated by default as identity,
    "CreatedAt" timestamp with time zone not null,
    "Login" text not null,
    "NormalizedLogin" text not null,
    "PasswordHash" text not null,
    "PasswordSalt" text not null,

    constraint "PK_Users" primary key ("Id")
);

create unique index "IX_Users_NormalizedUserName" ON "overwurd"."Users" ("NormalizedLogin");

create unique index "IX_Users_UserName" ON "overwurd"."Users" ("Login");

create table "overwurd"."Courses" (
    "Id" integer generated by default as identity,
    "CreatedAt" timestamp with time zone not null,
    "UserId" integer not null,
    "Name" text not null,
    "Description" text null,

    constraint "PK_Courses" primary key ("Id"),
    constraint "FK_Courses_Users_UserId" foreign key ("UserId") references overwurd."Users" ("Id") on delete restrict
);

create unique index "IX_Courses_Name_UserId" on "overwurd"."Courses" ("Name", "UserId");

create index "IX_Courses_UserId" on "overwurd"."Courses" ("UserId");